using Raylib_cs;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed class Text : Drawable<Text>, IFullCloneable
{
    public const int UnlimitedCharacters = -1;

    private Vector2? _sizeCache = null;

    public Text() { }

    public Text(Color fill)
    {
        Fill = fill;
    }

    public Text(string value)
    {
        Value = value;
    }

    public Text(string value, Color fill)
        : this(value)
    {
        Fill = fill;
    }

    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public int VisibleCharacters { get; set; } = UnlimitedCharacters;
    public Interpolation Interpolation { get; set; } = Drawing.DefaultInterpolation;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public string Value
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

    public Vector2 Size => _sizeCache ??= Font.MeasureText(Value, FontSize, Spacing, HeightMode);

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }

    protected override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawText(transform, this);
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
            int visibleCharacters = Text.UnlimitedCharacters,
            Interpolation? interpolation = null,
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
                interpolation,
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
            int visibleCharacters = Text.UnlimitedCharacters,
            Interpolation? interpolation = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            if (text.IsEmpty || colorValue == Color.Transparent)
                return;
            font ??= Font.Default;
            font.Atlas.Interpolation = interpolation ?? Drawing.DefaultInterpolation;
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
            int visibleCharacters = Text.UnlimitedCharacters,
            Interpolation? interpolation = null,
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
                interpolation,
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
            int visibleCharacters = Text.UnlimitedCharacters,
            Interpolation? interpolation = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
            var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
            if (text.IsEmpty || colorValue == Color.Transparent || strokeWidthValue <= 0)
                return;
            font ??= Font.Default;
            var (atlas, glyphInfos) = font.GetStroke((int)strokeWidthValue.Round());
            atlas.Interpolation = interpolation ?? Drawing.DefaultInterpolation;
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

        public void DrawText(Text text)
        {
            graphics.DrawText(new Transform(), text);
        }

        public void DrawText(float x, float y, Text text)
        {
            graphics.DrawText(new Vector2(x, y), text);
        }

        public void DrawText(Vector2 position, Text text)
        {
            graphics.DrawText(new Transform(position + text.Size * 0.5f), text);
        }

        public void DrawText(Transform transform, Text text)
        {
            text.OnBeginDrawing?.Invoke(transform, text, graphics);
            transform += text.Transform;
            var camera = text.Camera.Get();
            var value = text.Value;
            var fill = text.Fill;
            var stroke = text.Stroke;
            var font = text.Font;
            var fontSize = text.FontSize;
            var strokeWidth = text.StrokeWidth;
            var spacing = text.Spacing;
            var visibleCharacters = text.VisibleCharacters;
            var interpolation = text.Interpolation;
            var order = text.DrawOrder;
            var position = transform.Position;
            var scale = (transform.Scale.X.Abs() + transform.Scale.Y.Abs()) * 0.5f;
            var size = text.Size;
            fontSize *= scale;
            transform.Scale = size;
            graphics.PushMatrix();
            graphics.Pivot(transform, true);
            if (!graphics.Culling() || graphics.IsBoxInBounds(position, size * scale, camera, strokeWidth * 0.5f))
            {
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
                        interpolation,
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
                        interpolation,
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
                        interpolation,
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
                        interpolation,
                        camera
                    );
                }
            }

            graphics.PopMatrix();
            text.OnEndDrawing?.Invoke(transform, text, graphics);
        }
    }
}
