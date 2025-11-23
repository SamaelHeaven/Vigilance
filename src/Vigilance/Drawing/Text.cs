using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Text : IFullCloneable
{
    private Vector2? _sizeCache = null;

    public Text() { }

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
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;
    public Interpolation Interpolation { get; set; } = Drawing.DefaultInterpolation;
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;

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
        return ObjectPrinter.Print(this);
    }
}
