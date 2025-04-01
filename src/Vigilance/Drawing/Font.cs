using System.Runtime.InteropServices;
using FreeTypeSharp;
using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed unsafe class Font
{
    private static readonly FreeTypeLibrary FtLibrary = new();
    private readonly string _charset;
    private readonly Dictionary<char, GlyphInfo> _glyphInfos = new();
    private readonly int _quality;
    private readonly Dictionary<int, (Texture2D, Dictionary<char, GlyphInfo>)> _strokes = new();
    private IntPtr _buffer;
    private FT_FaceRec_* _face;
    private int _spaceSize;
    private FT_StrokerRec_* _stroker;

    public Font(byte[] bytes, int? quality = null, string? charset = null)
    {
        Game.EnsureRunning();
        _quality = quality ?? Game.DefaultFontQuality;
        _charset = string.Concat((charset ?? Game.DefaultFontCharset).Distinct());
        var glyphs = LoadGlyphs(bytes);
        Atlas = DrawAtlas(glyphs);
    }

    internal Texture2D Atlas { get; }

    public Vector2 MeasureText(string text, float fontSize, Vector2? spacing = null)
    {
        var (spacingX, spacingY) = (spacing ?? Game.DefaultTextSpacing).ToTuple();
        var size = new Vector2(0, fontSize + text.Count(c => c == '\n') * (fontSize + spacingY));
        HandleText(
            (_, _, destPosition, destSize) =>
            {
                size.X = MathF.Max(size.X, destPosition.X + destSize.X);
            },
            text,
            fontSize,
            (spacingX, spacingY)
        );
        return size;
    }

    internal void HandleText(
        Action<Vector2, Vector2, Vector2, Vector2> action,
        string text,
        float fontSize,
        Vector2 spacing,
        Dictionary<char, GlyphInfo>? glyphInfos = null
    )
    {
        var aspectRatio = _quality / fontSize;
        var position = Vector2.Zero;
        foreach (var c in text)
        {
            switch (c)
            {
                case '\n':
                    position.X = 0;
                    position.Y += fontSize + spacing.Y;
                    continue;
                case ' ':
                    position.X += _spaceSize / aspectRatio + spacing.X;
                    continue;
            }

            if (!(glyphInfos ?? _glyphInfos).TryGetValue(c, out var glyph))
                continue;
            var sourcePosition = new Vector2(glyph.X, glyph.Y);
            var sourceSize = new Vector2(glyph.Width, glyph.Height);
            var destPosition = position + new Vector2(glyph.OffsetX, glyph.OffsetY) / aspectRatio;
            var destSize = sourceSize / aspectRatio;
            action.Invoke(sourcePosition, sourceSize, destPosition, destSize);
            position.X += glyph.Advance / aspectRatio + spacing.X;
        }
    }

    private List<Glyph> LoadGlyphs(byte[] bytes)
    {
        _buffer = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, _buffer, bytes.Length);
        fixed (FT_FaceRec_** face = &_face)
        {
            FtEnsureOk(FT.FT_New_Memory_Face(FtLibrary.Native, (byte*)_buffer, bytes.Length, 0, face));
        }

        FtEnsureOk(FT.FT_Set_Char_Size(_face, 0, _quality * 64, 0, 0));
        FtEnsureOk(FT.FT_Load_Char(_face, ' ', FT_LOAD.FT_LOAD_DEFAULT));
        _spaceSize = _face->glyph->metrics.horiAdvance.ToInt32() / 64;
        fixed (FT_StrokerRec_** stroke = &_stroker)
        {
            FtEnsureOk(FT.FT_Stroker_New(FtLibrary.Native, stroke));
        }

        return _charset.Select(c => LoadGlyph(c, false)).Where(g => g.HasValue).Select(g => g!.Value).ToList();
    }

    private Texture2D DrawAtlas(List<Glyph> glyphs, Dictionary<char, GlyphInfo>? glyphInfos = null)
    {
        const int spacing = 2;
        var width = glyphs.Sum(glyph => glyph.Width) + (glyphs.Count - 1) * spacing + spacing * 2;
        var height = glyphs.Select(glyph => glyph.Height).Prepend(0).Max() + spacing * 2;
        var image = Raylib.GenImageColor(width, height, Raylib_cs.Color.Blank);
        var maxAscent = glyphs.Select(glyph => glyph.BearerY).Prepend(0).Max();
        var x = spacing;
        foreach (var glyph in glyphs)
        {
            for (var row = 0; row < glyph.Height; row++)
            for (var col = 0; col < glyph.Width; col++)
            {
                var alpha = glyph.Bitmap[row * glyph.Width + col];
                if (alpha == 255)
                    Raylib.ImageDrawPixel(ref image, x + col, row + spacing, Raylib_cs.Color.White);
            }

            (glyphInfos ?? _glyphInfos)[glyph.Character] = new GlyphInfo(
                x,
                spacing,
                glyph.Width,
                glyph.Height,
                glyph.Advance,
                glyph.BearerX,
                maxAscent - glyph.BearerY
            );
            x += glyph.Width + spacing;
        }

        var result = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);
        return result;
    }

    private Glyph? LoadGlyph(char c, bool stroke)
    {
        if (!stroke)
        {
            var error = FT.FT_Load_Char(_face, c, FT_LOAD.FT_LOAD_RENDER);
            if (error != FT_Error.FT_Err_Ok)
                return null;
        }

        var bitmap = _face->glyph->bitmap;
        if (stroke)
        {
            var index = FT.FT_Get_Char_Index(_face, c);
            FT.FT_Load_Glyph(_face, index, FT_LOAD.FT_LOAD_DEFAULT);
            FT_GlyphRec_* glyph;
            FtEnsureOk(FT.FT_Get_Glyph(_face->glyph, &glyph));
            FtEnsureOk(FT.FT_Glyph_Stroke(&glyph, _stroker, 1));
            FtEnsureOk(FT.FT_Glyph_To_Bitmap(&glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL, null, 1));
            bitmap = (*(FtBitmapGlyphRec*)glyph).Bitmap;
            FT.FT_Done_Glyph(glyph);
        }

        if (bitmap.buffer == null)
            return null;
        var bytes = new byte[bitmap.width * bitmap.rows];
        Marshal.Copy((IntPtr)bitmap.buffer, bytes, 0, (int)bitmap.width * (int)bitmap.rows);
        return new Glyph(
            bytes,
            c,
            (int)bitmap.width,
            (int)bitmap.rows,
            _face->glyph->advance.x.ToInt32() >> 6,
            _face->glyph->bitmap_left,
            _face->glyph->bitmap_top
        );
    }

    internal (Texture2D, Dictionary<char, GlyphInfo>) GetStroke(int strokeWidth)
    {
        strokeWidth = System.Math.Clamp(strokeWidth, 0, 50);
        if (_strokes.TryGetValue(strokeWidth, out var stroke))
            return stroke;
        FT.FT_Stroker_Set(
            _stroker,
            strokeWidth * 64,
            FT_Stroker_LineCap_.FT_STROKER_LINECAP_ROUND,
            FT_Stroker_LineJoin_.FT_STROKER_LINEJOIN_ROUND,
            0
        );
        var glyphs = _charset.Select(c => LoadGlyph(c, true)).Where(g => g.HasValue).Select(g => g!.Value).ToList();
        var glyphInfos = new Dictionary<char, GlyphInfo>();
        var atlas = DrawAtlas(glyphs, glyphInfos);
        var result = (atlas, glyphInfos);
        _strokes[strokeWidth] = result;
        return result;
    }

    private static void FtEnsureOk(FT_Error error)
    {
        if (error != FT_Error.FT_Err_Ok)
            throw new Exception("An error occurred while loading font data.");
    }

    ~Font()
    {
        Game.RunLater(() =>
        {
            FT.FT_Stroker_Done(_stroker);
            FT.FT_Done_Face(_face);
            Marshal.FreeHGlobal(_buffer);
            Raylib.UnloadTexture(Atlas);
            foreach (var stroke in _strokes)
                Raylib.UnloadTexture(stroke.Value.Item1);
        });
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FtBitmapGlyphRec
    {
        public FT_GlyphRec_ Root;
        public int Left;
        public int Top;
        public FT_Bitmap_ Bitmap;
    }
}
