using System.Runtime.InteropServices;
using FreeTypeSharp;
using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed unsafe class Font
{
    private const int AtlasSpacing = 4;
    private const int AtlasNbCols = 10;
    private static readonly FreeTypeLibrary FtLibrary = new();
    private readonly Dictionary<char, GlyphInfo> _glyphInfos = new();
    private readonly Dictionary<int, (Texture Atlas, Dictionary<char, GlyphInfo> GlyphInfos)> _strokes = new();
    private nint _buffer;
    private FT_FaceRec_* _face;
    private int _spaceSize;
    private FT_StrokerRec_* _stroker;

    public Font(IEnumerable<byte> bytes, int? quality = null, string? charset = null)
    {
        Game.EnsureRunning();
        Quality = quality ?? Game.DefaultFontQuality;
        Charset = string.Concat((charset ?? Game.DefaultFontCharset).Distinct());
        var glyphs = LoadGlyphs(bytes);
        Atlas = DrawAtlas(glyphs);
    }

    public string Charset { get; }
    public int Quality { get; }
    public IReadOnlyDictionary<char, GlyphInfo> GlyphInfos => _glyphInfos.AsReadOnly();
    public Texture Atlas { get; }

    public Vector2 MeasureText(string text, float fontSize, Vector2? spacing = null)
    {
        var (spacingX, spacingY) = spacing ?? Game.DefaultTextSpacing;
        var size = new Vector2(0, fontSize + text.Count(c => c == '\n') * (fontSize + spacingY));
        foreach (var (_, dest) in GetTextBounds(text, fontSize, new Vector2(spacingX, spacingY)))
            size.X = MathF.Max(size.X, dest.Position.X + dest.Size.X);
        return size;
    }

    public Texture GetStrokeAtlas(int strokeWidth)
    {
        var stroke = GetStroke(strokeWidth);
        return stroke.Atlas;
    }

    public GlyphInfo GetGlyphInfo(char c)
    {
        return _glyphInfos[c];
    }

    public GlyphInfo GetStrokeGlyphInfo(char c, int strokeWidth)
    {
        var stroke = GetStroke(strokeWidth);
        return stroke.GlyphInfos[c];
    }

    public IReadOnlyDictionary<char, GlyphInfo> GetStrokeGlyphInfos(int strokeWidth)
    {
        var stroke = GetStroke(strokeWidth);
        return stroke.GlyphInfos.AsReadOnly();
    }

    public IEnumerable<(Box Source, Box Dest)> GetTextBounds(
        string text,
        float? fontSize,
        Vector2? spacing,
        IReadOnlyDictionary<char, GlyphInfo>? glyphInfos = null
    )
    {
        return GetTextBounds(text, fontSize ?? Game.DefaultFontSize, spacing ?? Game.DefaultTextSpacing, glyphInfos);
    }

    public IEnumerable<(Box Source, Box Dest)> GetTextBounds(
        string text,
        float fontSize,
        Vector2 spacing,
        IReadOnlyDictionary<char, GlyphInfo>? glyphInfos = null
    )
    {
        var aspectRatio = Quality / fontSize;
        var position = Vector2.Zero;
        foreach (var c in text)
        {
            switch (c)
            {
                case '\n':
                    position.X = 0;
                    position.Y += fontSize + spacing.Y;
                    continue;
                case '\t':
                    position.X += (_spaceSize / aspectRatio + spacing.X) * 4;
                    continue;
                case ' ':
                    position.X += _spaceSize / aspectRatio + spacing.X;
                    continue;
            }

            if (!(glyphInfos ?? _glyphInfos).TryGetValue(c, out var glyph))
                continue;
            var atlasSpacing = glyph.Stroke == 0 ? 0 : AtlasSpacing;
            var halfAtlasSpacing = atlasSpacing * 0.5f;
            var sourcePosition = new Vector2(glyph.X, glyph.Y) - halfAtlasSpacing;
            var sourceSize = new Vector2(glyph.Width, glyph.Height) + atlasSpacing;
            var destPosition =
                position
                + new Vector2(
                    glyph.OffsetX - glyph.Stroke - halfAtlasSpacing,
                    glyph.OffsetY - glyph.Stroke - halfAtlasSpacing
                ) / aspectRatio;
            var destSize = sourceSize / aspectRatio;
            yield return (new Box(sourcePosition, sourceSize), new Box(destPosition, destSize));
            position.X += glyph.Advance / aspectRatio + spacing.X;
        }
    }

    private List<Glyph> LoadGlyphs(IEnumerable<byte> bytes)
    {
        var span = bytes.AsSpan();
        _buffer = Marshal.AllocHGlobal(span.Length);
        fixed (byte* bytesBuffer = span)
        {
            Buffer.MemoryCopy(bytesBuffer, (byte*)_buffer, span.Length, span.Length);
        }

        fixed (FT_FaceRec_** face = &_face)
        {
            FtEnsureOk(FT.FT_New_Memory_Face(FtLibrary.Native, (byte*)_buffer, span.Length, 0, face));
        }

        FtEnsureOk(FT.FT_Set_Char_Size(_face, 0, Quality * 64, 0, 0));
        FtEnsureOk(FT.FT_Load_Char(_face, ' ', FT_LOAD.FT_LOAD_DEFAULT));
        _spaceSize = _face->glyph->metrics.horiAdvance.ToInt32() / 64;
        fixed (FT_StrokerRec_** stroke = &_stroker)
        {
            FtEnsureOk(FT.FT_Stroker_New(FtLibrary.Native, stroke));
        }

        return Charset.Select(c => LoadGlyph(c, null)).Where(g => g.HasValue).Select(g => g!.Value).ToList();
    }

    private Texture DrawAtlas(List<Glyph> glyphs, Dictionary<char, GlyphInfo>? glyphInfos = null)
    {
        var colSize = glyphs.Select(glyph => glyph.Width).Prepend(0).Max();
        var rowSize = glyphs.Select(glyph => glyph.Height).Prepend(0).Max();
        var nbRows = (int)(glyphs.Count / (float)AtlasNbCols).Ceil();
        var width = AtlasNbCols * (colSize + AtlasSpacing) + AtlasSpacing;
        var height = nbRows * (rowSize + AtlasSpacing) + AtlasSpacing;
        var pixels = new byte[width * height * 2];
        var maxAscent = glyphs.Select(glyph => glyph.BearerY).Prepend(0).Max();
        var x = AtlasSpacing;
        var y = AtlasSpacing;
        var offset = 0;
        glyphInfos ??= _glyphInfos;
        foreach (var glyph in glyphs)
        {
            var glyphWidth = glyph.Width;
            var glyphHeight = glyph.Height;
            for (var i = 0; i < glyphWidth * glyphHeight; i++)
            {
                var row = i / glyphWidth;
                var col = i % glyphWidth;
                var alpha = glyph.Bitmap[i];
                if (alpha != 255)
                    continue;
                var px = x + col;
                var py = y + row;
                var index = (py * width + px) * 2;
                pixels[index] = 255;
                pixels[index + 1] = 255;
            }

            glyphInfos[glyph.Character] = new GlyphInfo(
                x,
                y,
                glyph.Width,
                glyph.Height,
                glyph.Advance,
                glyph.BearerX,
                maxAscent - glyph.BearerY,
                glyph.Stroke
            );
            x += colSize + AtlasSpacing;
            offset++;
            if (offset != AtlasNbCols)
                continue;
            x = AtlasSpacing;
            y += rowSize + AtlasSpacing;
            offset = 0;
        }

        fixed (byte* pixelsBuffer = pixels)
        {
            var result = new Texture2D
            {
                Width = width,
                Height = height,
                Format = PixelFormat.UncompressedGrayAlpha,
                Mipmaps = 1,
            };
            result.Id = Rlgl.LoadTexture(pixelsBuffer, result.Width, result.Height, result.Format, result.Mipmaps);
            return new Texture(result);
        }
    }

    private Glyph? LoadGlyph(char c, int? stroke)
    {
        if (!stroke.HasValue)
        {
            var error = FT.FT_Load_Char(_face, c, FT_LOAD.FT_LOAD_RENDER);
            if (error != FT_Error.FT_Err_Ok)
                return null;
        }

        var bitmap = _face->glyph->bitmap;
        if (stroke.HasValue)
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

        if (bitmap.buffer is null)
            return null;
        var bytes = new byte[bitmap.width * bitmap.rows];
        Marshal.Copy((nint)bitmap.buffer, bytes, 0, (int)bitmap.width * (int)bitmap.rows);
        return new Glyph(
            bytes,
            c,
            (int)bitmap.width,
            (int)bitmap.rows,
            _face->glyph->advance.x.ToInt32() >> 6,
            _face->glyph->bitmap_left,
            _face->glyph->bitmap_top,
            stroke ?? 0
        );
    }

    internal (Texture Atlas, Dictionary<char, GlyphInfo> GlyphInfos) GetStroke(int strokeWidth)
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
        var glyphs = Charset
            .Select(c => LoadGlyph(c, strokeWidth))
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();
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
        Game.Defer(() =>
        {
            FT.FT_Stroker_Done(_stroker);
            FT.FT_Done_Face(_face);
            Marshal.FreeHGlobal(_buffer);
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
