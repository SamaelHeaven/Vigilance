using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using FreeTypeSharp;

namespace Vigilance.Drawing;

public sealed unsafe class Font : IDisposable
{
    private const int AtlasSpacing = 4;
    private const int AtlasNbCols = 10;
    private static readonly FreeTypeLibrary _ftLibrary = new();
    private static FontConfig _config = new();
    private nint _buffer;
    private FT_FaceRec_* _face;
    private ValueDictionary<Rune, GlyphInfo> _glyphInfos = [];
    private int _spaceSize;
    private FT_StrokerRec_* _stroker;
    private ValueDictionary<int, StrokeEntry> _strokes = [];

    public Font(in ReadOnlySpan<byte> bytes, int? quality = null, string? charset = null)
    {
        Game.ThrowIfNotRunning();
        Quality = quality ?? DefaultQuality;
        Charset = string.Concat((charset ?? DefaultCharset).Distinct());
        var glyphs = LoadGlyphs(bytes);
        Atlas = DrawAtlas(glyphs, ref _glyphInfos);
    }

    public static Font Default { get; set; } = null!;

    public static int DefaultQuality { get; set; } = _config.DefaultQuality;

    public static int DefaultSize { get; set; } = _config.DefaultSize;

    public static TextHeightMode DefaultTextHeightMode { get; set; } = _config.DefaultTextHeightMode;

    public static Vector2 DefaultTextSpacing { get; set; } = _config.DefaultTextSpacing;

    public static string DefaultCharset { get; set; } = _config.DefaultCharset;

    public string Charset { get; private set; }
    public int Quality { get; private set; }
    public ValueDictionaryView<Rune, GlyphInfo> GlyphInfos => _glyphInfos;
    public Texture Atlas { get; private set; }
    public bool IsValid => _buffer != 0;

    public void Dispose()
    {
        ReleaseUnmanagedResources();
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
        _config = Game.Config.Take<FontConfig>() ?? _config;
        DefaultQuality = _config.DefaultQuality;
        DefaultSize = _config.DefaultSize;
        DefaultTextHeightMode = _config.DefaultTextHeightMode;
        DefaultTextSpacing = _config.DefaultTextSpacing;
        DefaultCharset = _config.DefaultCharset;
        Default = _config.Default.Invoke();
    }

    public Vector2 MeasureText(
        string text,
        float? fontSize = null,
        in Vector2? spacing = null,
        TextHeightMode? textHeightMode = null,
        int visibleCharacters = Text.UnlimitedCharacters
    )
    {
        var fontSizeValue = fontSize ?? DefaultSize;
        var spacingValue = spacing ?? DefaultTextSpacing;
        var size = Vector2.Zero;
        foreach (var (_, dest) in GetTextBounds(text, fontSizeValue, spacingValue, visibleCharacters))
            size = size.Max(dest.Position + dest.Size);
        if ((textHeightMode ?? DefaultTextHeightMode) == TextHeightMode.FontSize)
            size.Y = fontSizeValue + text.AsValueEnumerable().Count(c => c == '\n') * (fontSizeValue + spacingValue.Y);
        return size;
    }

    public Texture GetStrokeAtlas(int strokeWidth)
    {
        return GetStroke(strokeWidth).Atlas;
    }

    public GlyphInfo GetGlyphInfo(Rune rune)
    {
        return _glyphInfos[rune];
    }

    public GlyphInfo GetGlyphInfo(char c)
    {
        return _glyphInfos[new Rune(c)];
    }

    public GlyphInfo GetStrokeGlyphInfo(Rune rune, int strokeWidth)
    {
        return GetStroke(strokeWidth).GlyphInfos[rune];
    }

    public GlyphInfo GetStrokeGlyphInfo(char c, int strokeWidth)
    {
        return GetStroke(strokeWidth).GlyphInfos[new Rune(c)];
    }

    public ValueDictionaryView<Rune, GlyphInfo> GetStrokeGlyphInfos(int strokeWidth)
    {
        return GetStroke(strokeWidth).GlyphInfos;
    }

    public TextBoundEnumerable GetTextBounds(
        string text,
        float? fontSize = null,
        in Vector2? spacing = null,
        int visibleCharacters = Text.UnlimitedCharacters,
        ValueDictionaryView<Rune, GlyphInfo> glyphInfos = default
    )
    {
        return new TextBoundEnumerable(
            this,
            text,
            fontSize ?? DefaultSize,
            spacing ?? DefaultTextSpacing,
            (glyphInfos == default ? _glyphInfos : glyphInfos).AsEnumerable(),
            visibleCharacters
        );
    }

    public Stroke GetStroke(int strokeWidth)
    {
        strokeWidth = strokeWidth.Clamp(0, 50);
        ref var stroke = ref _strokes.GetValueRefOrAddDefault(strokeWidth, out var exists);
        if (exists)
#pragma warning disable CS9083 // A member is returned by reference but was initialized to a value that cannot be returned by reference
            return new Stroke(stroke.Atlas, stroke.GlyphInfos.AsView());
#pragma warning restore CS9083 // A member is returned by reference but was initialized to a value that cannot be returned by reference
        FT.FT_Stroker_Set(
            _stroker,
            strokeWidth * 64,
            FT_Stroker_LineCap_.FT_STROKER_LINECAP_ROUND,
            FT_Stroker_LineJoin_.FT_STROKER_LINEJOIN_ROUND,
            0
        );
        var glyphs = Charset
            .EnumerateRunes()
            .AsValueEnumerable()
            .Select(c => LoadGlyph(c, strokeWidth))
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToValueList();
        var glyphInfos = new ValueDictionary<Rune, GlyphInfo>();
        var atlas = DrawAtlas(glyphs, ref glyphInfos);
        stroke = new StrokeEntry(atlas, glyphInfos);
        var result = new Stroke(atlas, stroke.GlyphInfos.AsView());
#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope
        return result;
#pragma warning restore CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope
    }

    private ValueList<Glyph> LoadGlyphs(in ReadOnlySpan<byte> bytes)
    {
        _buffer = Marshal.AllocHGlobal(bytes.Length);
        fixed (byte* bytesBuffer = bytes)
        {
            Buffer.MemoryCopy(bytesBuffer, (byte*)_buffer, bytes.Length, bytes.Length);
        }

        fixed (FT_FaceRec_** face = &_face)
        {
            FtThrowIfError(FT.FT_New_Memory_Face(_ftLibrary.Native, (byte*)_buffer, bytes.Length, 0, face));
        }

        FtThrowIfError(FT.FT_Set_Char_Size(_face, 0, Quality * 64, 0, 0));
        FtThrowIfError(FT.FT_Load_Char(_face, ' ', FT_LOAD.FT_LOAD_DEFAULT));
        _spaceSize = _face->glyph->metrics.horiAdvance.ToInt32() / 64;
        fixed (FT_StrokerRec_** stroke = &_stroker)
        {
            FtThrowIfError(FT.FT_Stroker_New(_ftLibrary.Native, stroke));
        }

        return Charset
            .EnumerateRunes()
            .AsValueEnumerable()
            .Select(c => LoadGlyph(c, null))
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToValueList();
    }

    private static Texture DrawAtlas(in ValueList<Glyph> glyphs, ref ValueDictionary<Rune, GlyphInfo> glyphInfos)
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

    private Glyph? LoadGlyph(Rune rune, int? stroke)
    {
        if (!stroke.HasValue)
        {
            var error = FT.FT_Load_Char(_face, (nuint)rune.Value, FT_LOAD.FT_LOAD_RENDER);
            if (error != FT_Error.FT_Err_Ok)
                return null;
        }

        var bitmap = _face->glyph->bitmap;
        if (stroke.HasValue)
        {
            var index = FT.FT_Get_Char_Index(_face, (nuint)rune.Value);
            FT.FT_Load_Glyph(_face, index, FT_LOAD.FT_LOAD_DEFAULT);
            FT_GlyphRec_* glyph;
            FtThrowIfError(FT.FT_Get_Glyph(_face->glyph, &glyph));
            FtThrowIfError(FT.FT_Glyph_Stroke(&glyph, _stroker, 1));
            FtThrowIfError(FT.FT_Glyph_To_Bitmap(&glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL, null, 1));
            bitmap = (*(FtBitmapGlyphRec*)glyph).Bitmap;
            FT.FT_Done_Glyph(glyph);
        }

        if (bitmap.buffer is null)
            return null;
        var bytes = GC.AllocateUninitializedArray<byte>((int)(bitmap.width * bitmap.rows));
        Marshal.Copy((nint)bitmap.buffer, bytes, 0, bytes.Length);
        return new Glyph(
            bytes,
            rune,
            (int)bitmap.width,
            (int)bitmap.rows,
            _face->glyph->advance.x.ToInt32() >> 6,
            _face->glyph->bitmap_left,
            _face->glyph->bitmap_top,
            stroke ?? 0
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FtThrowIfError(FT_Error error)
    {
        if (error != FT_Error.FT_Err_Ok)
            throw new FreeTypeException(error);
    }

    ~Font()
    {
        Game.Defer(Dispose);
    }

    private void ReleaseUnmanagedResources()
    {
        FT.FT_Stroker_Done(_stroker);
        FT.FT_Done_Face(_face);
        Marshal.FreeHGlobal(_buffer);
    }

    private struct StrokeEntry(Texture atlas, in ValueDictionary<Rune, GlyphInfo> glyphInfos)
    {
        public readonly Texture Atlas = atlas;
        public readonly ValueDictionary<Rune, GlyphInfo> GlyphInfos = glyphInfos;
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
        private readonly ValueDictionaryView<Rune, GlyphInfo>.Enumerable _glyphInfos;
        private readonly int _visibleCharacters;

        internal TextBoundEnumerable(
            Font font,
            string text,
            float fontSize,
            Vector2 spacing,
            in ValueDictionaryView<Rune, GlyphInfo>.Enumerable glyphInfos,
            int visibleCharacters
        )
        {
            _font = font;
            _text = text;
            _fontSize = fontSize;
            _spacing = spacing;
            _glyphInfos = glyphInfos;
            _visibleCharacters = visibleCharacters;
        }

        public TextBoundEnumerator GetEnumerator()
        {
            return new TextBoundEnumerator(_font, _text, _fontSize, _spacing, _glyphInfos, _visibleCharacters);
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
        private readonly ValueDictionaryView<Rune, GlyphInfo>.Enumerable _glyphInfos;
        private readonly int _visibleCharacters;
        private readonly float _aspectRatio;
        private int _index;
        private Vector2 _position;

        internal TextBoundEnumerator(
            Font font,
            string text,
            float fontSize,
            Vector2 spacing,
            in ValueDictionaryView<Rune, GlyphInfo>.Enumerable glyphInfos,
            int visibleCharacters
        )
        {
            _font = font;
            _text = text;
            _fontSize = fontSize;
            _spacing = spacing;
            _glyphInfos = glyphInfos;
            _visibleCharacters = visibleCharacters;
            _aspectRatio = _font.Quality / fontSize;
            Reset();
        }

        public bool MoveNext()
        {
            while (_index < _text.Length && (_visibleCharacters < 0 || _index < _visibleCharacters))
            {
                Rune.DecodeFromUtf16(_text.AsSpan(_index), out var rune, out var charsConsumed);
                _index += charsConsumed;
                switch (rune.Value)
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

                if (!_glyphInfos.TryGetValue(rune, out var glyph))
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

    public readonly ref struct Stroke(Texture atlas, ValueDictionaryView<Rune, GlyphInfo> glyphInfos)
    {
        public Texture Atlas { get; } = atlas;
        public ValueDictionaryView<Rune, GlyphInfo> GlyphInfos { get; } = glyphInfos;

        public void Deconstruct(out Texture atlas, out ValueDictionaryView<Rune, GlyphInfo> glyphInfos)
        {
            atlas = Atlas;
            glyphInfos = GlyphInfos;
        }
    }
}
