using System.Runtime.InteropServices;

namespace Vigilance.Drawing;

[StructLayout(LayoutKind.Sequential)]
public record struct Color
{
    public static Color White => new(255, 255, 255);
    public static Color Black => new(0, 0, 0);
    public static Color Transparent => default;
    public static Color RayWhite => new(245, 245, 245);
    public static Color RayLightGray => new(200, 200, 200);
    public static Color RayGray => new(130, 130, 130);
    public static Color RayDarkGray => new(80, 80, 80);
    public static Color RayYellow => new(253, 249, 0);
    public static Color RayGold => new(255, 203, 0);
    public static Color RayOrange => new(255, 161, 0);
    public static Color RayPink => new(255, 109, 194);
    public static Color RayRed => new(230, 41, 55);
    public static Color RayMaroon => new(190, 33, 55);
    public static Color RayGreen => new(0, 228, 48);
    public static Color RayLime => new(0, 158, 47);
    public static Color RayDarkGreen => new(0, 117, 44);
    public static Color RaySkyBlue => new(102, 191, 255);
    public static Color RayBlue => new(0, 121, 241);
    public static Color RayDarkBlue => new(0, 82, 172);
    public static Color RayPurple => new(200, 122, 255);
    public static Color RayViolet => new(135, 60, 190);
    public static Color RayDarkPurple => new(112, 31, 126);
    public static Color RayBeige => new(211, 176, 131);
    public static Color RayBrown => new(127, 106, 79);
    public static Color RayDarkBrown => new(76, 63, 47);
    public static Color RayMagenta => new(255, 0, 255);
    public static Color Red50 => new(254, 242, 242);
    public static Color Red100 => new(255, 226, 226);
    public static Color Red200 => new(255, 201, 201);
    public static Color Red300 => new(255, 162, 162);
    public static Color Red400 => new(255, 100, 103);
    public static Color Red500 => new(251, 44, 54);
    public static Color Red600 => new(231, 0, 11);
    public static Color Red700 => new(193, 0, 7);
    public static Color Red800 => new(159, 7, 18);
    public static Color Red900 => new(130, 24, 26);
    public static Color Red950 => new(70, 8, 9);
    public static Color Red => Red500;
    public static Color Orange50 => new(255, 247, 237);
    public static Color Orange100 => new(255, 237, 212);
    public static Color Orange200 => new(255, 214, 167);
    public static Color Orange300 => new(255, 184, 106);
    public static Color Orange400 => new(255, 137, 4);
    public static Color Orange500 => new(255, 105, 0);
    public static Color Orange600 => new(245, 73, 0);
    public static Color Orange700 => new(202, 53, 0);
    public static Color Orange800 => new(159, 45, 0);
    public static Color Orange900 => new(126, 42, 12);
    public static Color Orange950 => new(68, 19, 6);
    public static Color Orange => Orange500;
    public static Color Amber50 => new(255, 251, 235);
    public static Color Amber100 => new(254, 243, 198);
    public static Color Amber200 => new(254, 230, 133);
    public static Color Amber300 => new(255, 210, 48);
    public static Color Amber400 => new(255, 185, 0);
    public static Color Amber500 => new(254, 154, 0);
    public static Color Amber600 => new(225, 113, 0);
    public static Color Amber700 => new(187, 77, 0);
    public static Color Amber800 => new(151, 60, 0);
    public static Color Amber900 => new(123, 51, 6);
    public static Color Amber950 => new(70, 25, 1);
    public static Color Amber => Amber500;
    public static Color Yellow50 => new(254, 252, 232);
    public static Color Yellow100 => new(254, 249, 194);
    public static Color Yellow200 => new(255, 240, 133);
    public static Color Yellow300 => new(255, 223, 32);
    public static Color Yellow400 => new(253, 199, 0);
    public static Color Yellow500 => new(240, 177, 0);
    public static Color Yellow600 => new(208, 135, 0);
    public static Color Yellow700 => new(166, 95, 0);
    public static Color Yellow800 => new(137, 75, 0);
    public static Color Yellow900 => new(115, 62, 10);
    public static Color Yellow950 => new(67, 32, 4);
    public static Color Yellow => Yellow500;
    public static Color Lime50 => new(247, 254, 231);
    public static Color Lime100 => new(236, 252, 202);
    public static Color Lime200 => new(216, 249, 153);
    public static Color Lime300 => new(187, 244, 81);
    public static Color Lime400 => new(154, 230, 0);
    public static Color Lime500 => new(124, 207, 0);
    public static Color Lime600 => new(94, 165, 0);
    public static Color Lime700 => new(73, 125, 0);
    public static Color Lime800 => new(60, 99, 0);
    public static Color Lime900 => new(53, 83, 14);
    public static Color Lime950 => new(25, 46, 3);
    public static Color Lime => Lime500;
    public static Color Green50 => new(240, 253, 244);
    public static Color Green100 => new(220, 252, 231);
    public static Color Green200 => new(185, 248, 207);
    public static Color Green300 => new(123, 241, 168);
    public static Color Green400 => new(5, 223, 114);
    public static Color Green500 => new(0, 201, 80);
    public static Color Green600 => new(0, 166, 62);
    public static Color Green700 => new(0, 130, 54);
    public static Color Green800 => new(1, 102, 48);
    public static Color Green900 => new(13, 84, 43);
    public static Color Green950 => new(3, 46, 21);
    public static Color Green => Green500;
    public static Color Emerald50 => new(236, 253, 245);
    public static Color Emerald100 => new(208, 250, 229);
    public static Color Emerald200 => new(164, 244, 207);
    public static Color Emerald300 => new(94, 233, 181);
    public static Color Emerald400 => new(0, 212, 146);
    public static Color Emerald500 => new(0, 188, 125);
    public static Color Emerald600 => new(0, 153, 102);
    public static Color Emerald700 => new(0, 122, 85);
    public static Color Emerald800 => new(0, 96, 69);
    public static Color Emerald900 => new(0, 79, 59);
    public static Color Emerald950 => new(0, 44, 34);
    public static Color Emerald => Emerald500;
    public static Color Teal50 => new(240, 253, 250);
    public static Color Teal100 => new(203, 251, 241);
    public static Color Teal200 => new(150, 247, 228);
    public static Color Teal300 => new(70, 236, 213);
    public static Color Teal400 => new(0, 213, 190);
    public static Color Teal500 => new(0, 187, 167);
    public static Color Teal600 => new(0, 150, 137);
    public static Color Teal700 => new(0, 120, 111);
    public static Color Teal800 => new(0, 95, 90);
    public static Color Teal900 => new(11, 79, 74);
    public static Color Teal950 => new(2, 47, 46);
    public static Color Teal => Teal500;
    public static Color Cyan50 => new(236, 254, 255);
    public static Color Cyan100 => new(206, 250, 254);
    public static Color Cyan200 => new(162, 244, 253);
    public static Color Cyan300 => new(83, 234, 253);
    public static Color Cyan400 => new(0, 211, 242);
    public static Color Cyan500 => new(0, 184, 219);
    public static Color Cyan600 => new(0, 146, 184);
    public static Color Cyan700 => new(0, 117, 149);
    public static Color Cyan800 => new(0, 95, 120);
    public static Color Cyan900 => new(16, 78, 100);
    public static Color Cyan950 => new(5, 51, 69);
    public static Color Cyan => Cyan500;
    public static Color Sky50 => new(240, 249, 255);
    public static Color Sky100 => new(223, 242, 254);
    public static Color Sky200 => new(184, 230, 254);
    public static Color Sky300 => new(116, 212, 255);
    public static Color Sky400 => new(0, 188, 255);
    public static Color Sky500 => new(0, 166, 244);
    public static Color Sky600 => new(0, 132, 209);
    public static Color Sky700 => new(0, 105, 168);
    public static Color Sky800 => new(0, 89, 138);
    public static Color Sky900 => new(2, 74, 112);
    public static Color Sky950 => new(5, 47, 74);
    public static Color Sky => Sky500;
    public static Color Blue50 => new(239, 246, 255);
    public static Color Blue100 => new(219, 234, 254);
    public static Color Blue200 => new(190, 219, 255);
    public static Color Blue300 => new(142, 197, 255);
    public static Color Blue400 => new(81, 162, 255);
    public static Color Blue500 => new(43, 127, 255);
    public static Color Blue600 => new(21, 93, 252);
    public static Color Blue700 => new(20, 71, 230);
    public static Color Blue800 => new(25, 60, 184);
    public static Color Blue900 => new(28, 57, 142);
    public static Color Blue950 => new(22, 36, 86);
    public static Color Blue => Blue500;
    public static Color Indigo50 => new(238, 242, 255);
    public static Color Indigo100 => new(224, 231, 255);
    public static Color Indigo200 => new(198, 210, 255);
    public static Color Indigo300 => new(163, 179, 255);
    public static Color Indigo400 => new(124, 134, 255);
    public static Color Indigo500 => new(97, 95, 255);
    public static Color Indigo600 => new(79, 57, 246);
    public static Color Indigo700 => new(67, 45, 215);
    public static Color Indigo800 => new(55, 42, 172);
    public static Color Indigo900 => new(49, 44, 133);
    public static Color Indigo950 => new(30, 26, 77);
    public static Color Indigo => Indigo500;
    public static Color Violet50 => new(245, 243, 255);
    public static Color Violet100 => new(237, 233, 254);
    public static Color Violet200 => new(221, 214, 255);
    public static Color Violet300 => new(196, 180, 255);
    public static Color Violet400 => new(166, 132, 255);
    public static Color Violet500 => new(142, 81, 255);
    public static Color Violet600 => new(127, 34, 254);
    public static Color Violet700 => new(112, 8, 231);
    public static Color Violet800 => new(93, 14, 192);
    public static Color Violet900 => new(77, 23, 154);
    public static Color Violet950 => new(47, 13, 104);
    public static Color Violet => Violet500;
    public static Color Purple50 => new(250, 245, 255);
    public static Color Purple100 => new(243, 232, 255);
    public static Color Purple200 => new(233, 212, 255);
    public static Color Purple300 => new(218, 178, 255);
    public static Color Purple400 => new(194, 122, 255);
    public static Color Purple500 => new(173, 70, 255);
    public static Color Purple600 => new(152, 16, 250);
    public static Color Purple700 => new(130, 0, 219);
    public static Color Purple800 => new(110, 17, 176);
    public static Color Purple900 => new(89, 22, 139);
    public static Color Purple950 => new(60, 3, 102);
    public static Color Purple => Purple500;
    public static Color Fuchsia50 => new(253, 244, 255);
    public static Color Fuchsia100 => new(250, 232, 255);
    public static Color Fuchsia200 => new(246, 207, 255);
    public static Color Fuchsia300 => new(244, 168, 255);
    public static Color Fuchsia400 => new(237, 106, 255);
    public static Color Fuchsia500 => new(225, 42, 251);
    public static Color Fuchsia600 => new(200, 0, 222);
    public static Color Fuchsia700 => new(168, 0, 183);
    public static Color Fuchsia800 => new(138, 1, 148);
    public static Color Fuchsia900 => new(114, 19, 120);
    public static Color Fuchsia950 => new(75, 0, 79);
    public static Color Fuchsia => Fuchsia500;
    public static Color Pink50 => new(253, 242, 248);
    public static Color Pink100 => new(252, 231, 243);
    public static Color Pink200 => new(252, 206, 232);
    public static Color Pink300 => new(253, 165, 213);
    public static Color Pink400 => new(251, 100, 182);
    public static Color Pink500 => new(246, 51, 154);
    public static Color Pink600 => new(230, 0, 118);
    public static Color Pink700 => new(198, 0, 92);
    public static Color Pink800 => new(163, 0, 76);
    public static Color Pink900 => new(134, 16, 67);
    public static Color Pink950 => new(81, 4, 36);
    public static Color Pink => Pink500;
    public static Color Rose50 => new(255, 241, 242);
    public static Color Rose100 => new(255, 228, 230);
    public static Color Rose200 => new(255, 204, 211);
    public static Color Rose300 => new(255, 161, 173);
    public static Color Rose400 => new(255, 99, 126);
    public static Color Rose500 => new(255, 32, 86);
    public static Color Rose600 => new(236, 0, 63);
    public static Color Rose700 => new(199, 0, 54);
    public static Color Rose800 => new(165, 0, 54);
    public static Color Rose900 => new(139, 8, 54);
    public static Color Rose950 => new(77, 2, 24);
    public static Color Rose => Rose500;
    public static Color Stone50 => new(250, 250, 249);
    public static Color Stone100 => new(245, 245, 244);
    public static Color Stone200 => new(231, 229, 228);
    public static Color Stone300 => new(214, 211, 209);
    public static Color Stone400 => new(166, 160, 155);
    public static Color Stone500 => new(121, 113, 107);
    public static Color Stone600 => new(87, 83, 77);
    public static Color Stone700 => new(68, 64, 59);
    public static Color Stone800 => new(41, 37, 36);
    public static Color Stone900 => new(28, 25, 23);
    public static Color Stone950 => new(12, 10, 9);
    public static Color Stone => Stone500;
    public static Color Neutral50 => new(250, 250, 250);
    public static Color Neutral100 => new(245, 245, 245);
    public static Color Neutral200 => new(229, 229, 229);
    public static Color Neutral300 => new(212, 212, 212);
    public static Color Neutral400 => new(161, 161, 161);
    public static Color Neutral500 => new(115, 115, 115);
    public static Color Neutral600 => new(82, 82, 82);
    public static Color Neutral700 => new(64, 64, 64);
    public static Color Neutral800 => new(38, 38, 38);
    public static Color Neutral900 => new(23, 23, 23);
    public static Color Neutral950 => new(10, 10, 10);
    public static Color Neutral => Neutral500;
    public static Color Zinc50 => new(250, 250, 250);
    public static Color Zinc100 => new(244, 244, 245);
    public static Color Zinc200 => new(228, 228, 231);
    public static Color Zinc300 => new(212, 212, 216);
    public static Color Zinc400 => new(159, 159, 169);
    public static Color Zinc500 => new(113, 113, 123);
    public static Color Zinc600 => new(82, 82, 92);
    public static Color Zinc700 => new(63, 63, 70);
    public static Color Zinc800 => new(39, 39, 42);
    public static Color Zinc900 => new(24, 24, 27);
    public static Color Zinc950 => new(9, 9, 11);
    public static Color Zinc => Zinc500;
    public static Color Gray50 => new(249, 250, 251);
    public static Color Gray100 => new(243, 244, 246);
    public static Color Gray200 => new(229, 231, 235);
    public static Color Gray300 => new(209, 213, 220);
    public static Color Gray400 => new(153, 161, 175);
    public static Color Gray500 => new(106, 114, 130);
    public static Color Gray600 => new(74, 85, 101);
    public static Color Gray700 => new(54, 65, 83);
    public static Color Gray800 => new(30, 41, 57);
    public static Color Gray900 => new(16, 24, 40);
    public static Color Gray950 => new(3, 7, 18);
    public static Color Gray => Gray500;
    public static Color Slate50 => new(248, 250, 252);
    public static Color Slate100 => new(241, 245, 249);
    public static Color Slate200 => new(226, 232, 240);
    public static Color Slate300 => new(202, 213, 226);
    public static Color Slate400 => new(144, 161, 185);
    public static Color Slate500 => new(98, 116, 142);
    public static Color Slate600 => new(69, 85, 108);
    public static Color Slate700 => new(49, 65, 88);
    public static Color Slate800 => new(29, 41, 61);
    public static Color Slate900 => new(15, 23, 43);
    public static Color Slate950 => new(2, 6, 24);
    public static Color Slate => Slate500;

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
            R = (byte)(hex >> 24 & 0xff),
            G = (byte)(hex >> 16 & 0xff),
            B = (byte)(hex >> 8 & 0xff),
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
            value = (byte)(hi << 4 | lo);
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

    public static Color Lerp(Color start, Color end, float t)
    {
        Color result = default;
        t = t.Clamp(0, 1);
        result.R = (byte)((1.0f - t) * start.R + t * end.R);
        result.G = (byte)((1.0f - t) * start.G + t * end.G);
        result.B = (byte)((1.0f - t) * start.B + t * end.B);
        result.A = (byte)((1.0f - t) * start.A + t * end.A);
        return result;
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

    internal readonly Raylib_cs.Color RColor => new(R, G, B, A);

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
        return (uint)R << 24 | (uint)G << 16 | (uint)B << 8 | A;
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

    public readonly Vector4 Normalize()
    {
        return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
    }

    public readonly Color Or(Color value)
    {
        return this == default ? value : this;
    }
}
