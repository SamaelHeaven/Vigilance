using System.Runtime.InteropServices;
using FreeTypeSharp;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Drawing;

public sealed unsafe class Font : IDisposable
{
    private const int AtlasSpacing = 4;
    private const int AtlasNbCols = 10;
    private static readonly FreeTypeLibrary _ftLibrary = new();
    private static FontConfig _config = new();
    private readonly Dictionary<char, GlyphInfo> _glyphInfos = new();
    private readonly Dictionary<int, (Texture Atlas, Dictionary<char, GlyphInfo> GlyphInfos)> _strokes = new();
    private nint _buffer;
    private FT_FaceRec_* _face;
    private int _spaceSize;
    private FT_StrokerRec_* _stroker;

    public Font(IEnumerable<byte> bytes, int? quality = null, string? charset = null)
    {
        Game.EnsureRunning();
        Quality = quality ?? DefaultQuality;
        Charset = string.Concat((charset ?? DefaultCharset).Distinct());
        var glyphs = LoadGlyphs(bytes);
        Atlas = DrawAtlas(glyphs);
    }

    public static Font Default { get; set; } = null!;

    public static int DefaultQuality
    {
        get => _config.DefaultQuality;
        set => _config.DefaultQuality = value;
    }

    public static int DefaultSize
    {
        get => _config.DefaultSize;
        set => _config.DefaultSize = value;
    }

    public static TextHeightMode DefaultTextHeightMode
    {
        get => _config.DefaultTextHeightMode;
        set => _config.DefaultTextHeightMode = value;
    }

    public static Vector2 DefaultTextSpacing
    {
        get => _config.DefaultTextSpacing;
        set => _config.DefaultTextSpacing = value;
    }

    public static string DefaultCharset
    {
        get => _config.DefaultCharset;
        set => _config.DefaultCharset = value;
    }

    public string Charset { get; private set; }
    public int Quality { get; private set; }
    public DictionaryView<char, GlyphInfo> GlyphInfos => _glyphInfos;
    public Texture Atlas { get; private set; }
    public bool Valid => _buffer != 0;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        _glyphInfos.Clear();
        _strokes.Clear();
        _buffer = 0;
        _face = null;
        _spaceSize = 0;
        _stroker = null;
        Charset = "";
        Quality = 0;
        Atlas = Texture.Empty;
    }

    internal static void Initialize()
    {
        if (Game.Config.TryTake(out FontConfig config))
            _config = config;
        Default = _config.Default.Invoke();
    }

    public Vector2 MeasureText(
        string text,
        float? fontSize = null,
        Vector2? spacing = null,
        TextHeightMode? textHeightMode = null
    )
    {
        var fontSizeValue = fontSize ?? DefaultSize;
        var spacingValue = spacing ?? DefaultTextSpacing;
        var size = Vector2.Zero;
        foreach (var (_, dest) in GetTextBounds(text, fontSizeValue, spacingValue))
            size = size.Max(dest.Position + dest.Size);
        if ((textHeightMode ?? DefaultTextHeightMode) == TextHeightMode.FontSize)
            size.Y = fontSizeValue + text.AsValueEnumerable().Count(c => c == '\n') * (fontSizeValue + spacingValue.Y);
        return size;
    }

    public Texture GetStrokeAtlas(int strokeWidth)
    {
        return GetStroke(strokeWidth).Atlas;
    }

    public GlyphInfo GetGlyphInfo(char c)
    {
        return _glyphInfos[c];
    }

    public GlyphInfo GetStrokeGlyphInfo(char c, int strokeWidth)
    {
        return GetStroke(strokeWidth).GlyphInfos[c];
    }

    public DictionaryView<char, GlyphInfo> GetStrokeGlyphInfos(int strokeWidth)
    {
        return GetStroke(strokeWidth).GlyphInfos;
    }

    public TextBoundEnumerable GetTextBounds(
        string text,
        float? fontSize = null,
        Vector2? spacing = null,
        DictionaryView<char, GlyphInfo>? glyphInfos = null
    )
    {
        return new TextBoundEnumerable(this, text, fontSize ?? DefaultSize, spacing ?? DefaultTextSpacing, glyphInfos);
    }

    public (Texture Atlas, DictionaryView<char, GlyphInfo> GlyphInfos) GetStroke(int strokeWidth)
    {
        strokeWidth = int.Clamp(strokeWidth, 0, 50);
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
            .AsValueEnumerable()
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
            FtEnsureOk(FT.FT_New_Memory_Face(_ftLibrary.Native, (byte*)_buffer, span.Length, 0, face));
        }

        FtEnsureOk(FT.FT_Set_Char_Size(_face, 0, Quality * 64, 0, 0));
        FtEnsureOk(FT.FT_Load_Char(_face, ' ', FT_LOAD.FT_LOAD_DEFAULT));
        _spaceSize = _face->glyph->metrics.horiAdvance.ToInt32() / 64;
        fixed (FT_StrokerRec_** stroke = &_stroker)
        {
            FtEnsureOk(FT.FT_Stroker_New(_ftLibrary.Native, stroke));
        }

        return Charset
            .AsValueEnumerable()
            .Select(c => LoadGlyph(c, null))
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();
    }

    private Texture DrawAtlas(List<Glyph> glyphs, Dictionary<char, GlyphInfo>? glyphInfos = null)
    {
        var colSize = glyphs.AsValueEnumerable().Select(glyph => glyph.Width).Prepend(0).Max();
        var rowSize = glyphs.AsValueEnumerable().Select(glyph => glyph.Height).Prepend(0).Max();
        var nbRows = (int)(glyphs.Count / (float)AtlasNbCols).Ceil();
        var width = AtlasNbCols * (colSize + AtlasSpacing) + AtlasSpacing;
        var height = nbRows * (rowSize + AtlasSpacing) + AtlasSpacing;
        var image = new WritableImage<PixelGrayAlpha>(width, height);
        var maxAscent = glyphs.AsValueEnumerable().Select(glyph => glyph.BearerY).Prepend(0).Max();
        var x = AtlasSpacing;
        var y = AtlasSpacing;
        var offset = 0;
        glyphInfos ??= _glyphInfos;
        foreach (var (bitmap, character, glyphWidth, glyphHeight, advance, bearerX, bearerY, stroke) in glyphs)
        {
            for (var i = 0; i < glyphWidth * glyphHeight; i++)
            {
                var row = i / glyphWidth;
                var col = i % glyphWidth;
                var alpha = bitmap[i];
                if (alpha != 255)
                    continue;
                var px = x + col;
                var py = y + row;
                image[px, py] = new PixelGrayAlpha(255);
            }

            glyphInfos[character] = new GlyphInfo(
                x,
                y,
                glyphWidth,
                glyphHeight,
                advance,
                bearerX,
                maxAscent - bearerY,
                stroke
            );
            x += colSize + AtlasSpacing;
            offset++;
            if (offset != AtlasNbCols)
                continue;
            x = AtlasSpacing;
            y += rowSize + AtlasSpacing;
            offset = 0;
        }

        return image.ToTexture();
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

    private static void FtEnsureOk(FT_Error error)
    {
        if (error != FT_Error.FT_Err_Ok)
            throw new Exception("An error occurred while loading font data.");
    }

    ~Font()
    {
        Game.Defer(() => Dispose(false));
    }

    private void ReleaseUnmanagedResources()
    {
        FT.FT_Stroker_Done(_stroker);
        FT.FT_Done_Face(_face);
        Marshal.FreeHGlobal(_buffer);
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
            Atlas.Dispose();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FtBitmapGlyphRec
    {
        public FT_GlyphRec_ Root;
        public int Left;
        public int Top;
        public FT_Bitmap_ Bitmap;
    }

    public readonly struct TextBoundEnumerable : IStructEnumerable<TextBoundEnumerator, (Box Source, Box Dest)>
    {
        private readonly Font _font;
        private readonly string _text;
        private readonly float _fontSize;
        private readonly Vector2 _spacing;
        private readonly DictionaryView<char, GlyphInfo> _glyphInfos;

        internal TextBoundEnumerable(
            Font font,
            string text,
            float fontSize,
            Vector2 spacing,
            DictionaryView<char, GlyphInfo>? glyphInfos
        )
        {
            _font = font;
            _text = text;
            _fontSize = fontSize;
            _spacing = spacing;
            _glyphInfos = glyphInfos ?? font.GlyphInfos;
        }

        public TextBoundEnumerator GetEnumerator()
        {
            return new TextBoundEnumerator(_font, _text, _fontSize, _spacing, _glyphInfos);
        }

        public ValueEnumerable<
            StructEnumerator<TextBoundEnumerator, (Box Source, Box Dest)>,
            (Box Source, Box Dest)
        > AsValueEnumerable()
        {
            return new StructEnumerator<TextBoundEnumerator, (Box Source, Box Dest)>(GetEnumerator());
        }
    }

    public struct TextBoundEnumerator : IStructEnumerator<(Box Source, Box Dest)>
    {
        private readonly Font _font;
        private readonly string _text;
        private readonly float _fontSize;
        private readonly Vector2 _spacing;
        private readonly DictionaryView<char, GlyphInfo> _glyphInfos;
        private readonly float _aspectRatio;
        private int _index;
        private Vector2 _position;

        internal TextBoundEnumerator(
            Font font,
            string text,
            float fontSize,
            Vector2 spacing,
            DictionaryView<char, GlyphInfo>? glyphInfos
        )
        {
            _font = font;
            _text = text;
            _fontSize = fontSize;
            _spacing = spacing;
            _glyphInfos = glyphInfos ?? font.GlyphInfos;
            _aspectRatio = _font.Quality / fontSize;
            Reset();
        }

        public bool MoveNext()
        {
            while (_index < _text.Length)
            {
                var c = _text[_index++];
                switch (c)
                {
                    case '\n':
                        _position.X = 0;
                        _position.Y += _fontSize + _spacing.Y;
                        continue;

                    case '\t':
                        _position.X += (_font._spaceSize / _aspectRatio + _spacing.X) * 4;
                        continue;

                    case ' ':
                        _position.X += _font._spaceSize / _aspectRatio + _spacing.X;
                        continue;
                }

                if (!_glyphInfos.TryGetValue(c, out var glyph))
                    continue;
                var atlasSpacing = glyph.Stroke == 0 ? 0 : 4;
                var halfAtlasSpacing = atlasSpacing * 0.5f;
                var sourcePosition = new Vector2(glyph.X, glyph.Y) - halfAtlasSpacing;
                var sourceSize = new Vector2(glyph.Width, glyph.Height) + atlasSpacing;
                var destPosition =
                    _position
                    + new Vector2(
                        glyph.OffsetX - glyph.Stroke - halfAtlasSpacing,
                        glyph.OffsetY - glyph.Stroke - halfAtlasSpacing
                    ) / _aspectRatio;
                var destSize = sourceSize / _aspectRatio;
                Current = (new Box(sourcePosition, sourceSize), new Box(destPosition, destSize));
                _position.X += glyph.Advance / _aspectRatio + _spacing.X;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _index = 0;
            _position = Vector2.Zero;
            Current = default;
        }

        public (Box Source, Box Dest) Current { get; private set; }

        public void Dispose() { }
    }
}
