using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

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

    public override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawText(transform, this);
    }
}
