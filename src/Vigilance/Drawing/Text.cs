using System.Runtime.CompilerServices;
using Raylib_cs;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

[ValueWrapper(typeof(Drawable<ValueText>), "Drawable")]
public partial struct ValueText : IDrawable
{
    private Vector2? _sizeCache = null;

    public ValueText(Color fill)
        : this()
    {
        Fill = fill;
    }

    public ValueText(string content)
        : this()
    {
        Content = content;
    }

    public ValueText(string content, Color fill)
        : this(content)
    {
        Fill = fill;
    }

    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public int VisibleCharacters { get; set; } = Font.UnlimitedCharacters;
    public TextureFilter TextureFilter { get; set; } = Drawing.DefaultTextureFilter;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public string Content
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            _sizeCache = null;
        }
    } = "";

    public Font Font
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            _sizeCache = null;
        }
    } = Font.Default;

    public float FontSize
    {
        get;
        set
        {
            if (Precision.AreEqual(field, value))
                return;
            field = value;
            _sizeCache = null;
        }
    } = Font.DefaultSize;

    public Vector2 Spacing
    {
        get;
        set
        {
            if (Precision.AreEqual(field, value))
                return;
            field = value;
            _sizeCache = null;
        }
    } = Font.DefaultTextSpacing;

    public TextHeightMode HeightMode
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            _sizeCache = null;
        }
    } = Font.DefaultTextHeightMode;

    public readonly Vector2 Size =>
        Unsafe.AsRef(in this)._sizeCache ??= Font.MeasureText(Content, FontSize, Spacing, HeightMode);

    public override readonly string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }

    public readonly void Draw(Transform transform, Graphics graphics)
    {
        graphics.DrawText(transform, this);
    }
}

[ValueWrapper(typeof(ValueText))]
public sealed partial class Text : IDrawable, IFullCloneable
{
    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }
}

public static class TextExtensions
{
    extension(Graphics graphics)
    {
        public void FillText(
            string text,
            float x,
            float y,
            Color? color = null,
            Font? font = null,
            float? fontSize = null,
            in Vector2? spacing = null,
            int visibleCharacters = Font.UnlimitedCharacters,
            TextureFilter? textureFilter = null,
            Camera? camera = null
        )
        {
            graphics.FillText(
                text,
                new Vector2(x, y),
                color,
                font,
                fontSize,
                spacing,
                visibleCharacters,
                textureFilter,
                camera
            );
        }

        public void FillText(
            string text,
            Vector2 position,
            Color? color = null,
            Font? font = null,
            float? fontSize = null,
            in Vector2? spacing = null,
            int visibleCharacters = Font.UnlimitedCharacters,
            TextureFilter? textureFilter = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            if (text.IsEmpty || colorValue == Color.Transparent)
                return;
            font ??= Font.Default;
            font.Atlas.TextureFilter = textureFilter ?? Drawing.DefaultTextureFilter;
            graphics.BeginDrawing(camera);
            foreach (var (source, dest) in font.GetTextBounds(text, fontSize, spacing, visibleCharacters))
            {
                var finalDest = new Box(
                    dest.Position.X + position.X,
                    dest.Position.Y + position.Y,
                    dest.Size.X,
                    dest.Size.Y
                );
                if (graphics.Culling() && !graphics.IsBoxInBounds(finalDest, camera))
                    continue;
                Raylib.DrawTexturePro(
                    font.Atlas.Texture2D,
                    new Raylib_cs.Rectangle(source.X, source.Y, source.Width, source.Height),
                    new Raylib_cs.Rectangle(
                        finalDest.Position.X,
                        finalDest.Position.Y,
                        finalDest.Size.X,
                        finalDest.Size.Y
                    ),
                    Vector2.Zero,
                    0,
                    colorValue.RColor
                );
            }

            graphics.EndDrawing();
        }

        public void StrokeText(
            string text,
            float x,
            float y,
            Color? color = null,
            Font? font = null,
            float? fontSize = null,
            float? strokeWidth = null,
            in Vector2? spacing = null,
            int visibleCharacters = Font.UnlimitedCharacters,
            TextureFilter? textureFilter = null,
            Camera? camera = null
        )
        {
            graphics.StrokeText(
                text,
                new Vector2(x, y),
                color,
                font,
                fontSize,
                strokeWidth,
                spacing,
                visibleCharacters,
                textureFilter,
                camera
            );
        }

        public void StrokeText(
            string text,
            Vector2 position,
            Color? color = null,
            Font? font = null,
            float? fontSize = null,
            float? strokeWidth = null,
            in Vector2? spacing = null,
            int visibleCharacters = Font.UnlimitedCharacters,
            TextureFilter? textureFilter = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
            var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
            if (text.IsEmpty || colorValue == Color.Transparent || strokeWidthValue <= 0)
                return;
            font ??= Font.Default;
            var (atlas, glyphInfos) = font.GetStroke((int)strokeWidthValue.Round());
            atlas.TextureFilter = textureFilter ?? Drawing.DefaultTextureFilter;
            graphics.BeginDrawing(camera);
            foreach (var (source, dest) in font.GetTextBounds(text, fontSize, spacing, visibleCharacters, glyphInfos))
            {
                var finalDest = new Box(
                    dest.Position.X + position.X,
                    dest.Position.Y + position.Y,
                    dest.Size.X,
                    dest.Size.Y
                );
                if (graphics.Culling() && !graphics.IsBoxInBounds(finalDest, camera))
                    continue;
                Raylib.DrawTexturePro(
                    atlas.Texture2D,
                    new Raylib_cs.Rectangle(source.X, source.Y, source.Width, source.Height),
                    new Raylib_cs.Rectangle(
                        finalDest.Position.X,
                        finalDest.Position.Y,
                        finalDest.Size.X,
                        finalDest.Size.Y
                    ),
                    Vector2.Zero,
                    0,
                    colorValue.RColor
                );
            }

            graphics.EndDrawing();
        }

        public void DrawText(in ValueText text)
        {
            graphics.DrawText(new Transform(), text);
        }

        public void DrawText(float x, float y, in ValueText text)
        {
            graphics.DrawText(new Vector2(x, y), text);
        }

        public void DrawText(Vector2 position, in ValueText text)
        {
            graphics.DrawText(new Transform(position + text.Size * 0.5f), text);
        }

        public void DrawText(Transform transform, in ValueText text)
        {
            using var _ = Drawable<ValueText>.EnterDrawing(ref transform, text.Drawable, text, graphics);
            var camera = text.Camera.Get();
            var value = text.Content;
            var fill = text.Fill;
            var stroke = text.Stroke;
            var font = text.Font;
            var fontSize = text.FontSize;
            var strokeWidth = text.StrokeWidth;
            var spacing = text.Spacing;
            var visibleCharacters = text.VisibleCharacters;
            var textureFilter = text.TextureFilter;
            var order = text.DrawOrder;
            var position = transform.Position;
            var scale = (transform.Scale.X.Abs() + transform.Scale.Y.Abs()) * 0.5f;
            var size = text.Size;
            fontSize *= scale;
            var pivotTransform = transform;
            pivotTransform.Scale = size;
            graphics.Pivot(pivotTransform, true);
            if (graphics.Culling() && !graphics.IsBoxInBounds(position, size * scale, camera, strokeWidth * 0.5f))
                return;
            if (order == DrawOrder.StrokeThenFill)
            {
                graphics.StrokeText(
                    value,
                    position,
                    stroke,
                    font,
                    fontSize,
                    strokeWidth,
                    spacing,
                    visibleCharacters,
                    textureFilter,
                    camera
                );
                graphics.FillText(
                    value,
                    position,
                    fill,
                    font,
                    fontSize,
                    spacing,
                    visibleCharacters,
                    textureFilter,
                    camera
                );
            }
            else
            {
                graphics.FillText(
                    value,
                    position,
                    fill,
                    font,
                    fontSize,
                    spacing,
                    visibleCharacters,
                    textureFilter,
                    camera
                );
                graphics.StrokeText(
                    value,
                    position,
                    stroke,
                    font,
                    fontSize,
                    strokeWidth,
                    spacing,
                    visibleCharacters,
                    textureFilter,
                    camera
                );
            }
        }
    }
}
