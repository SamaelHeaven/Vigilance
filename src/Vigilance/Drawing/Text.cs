using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Text : IFullCloneable
{
    private Font _font = Font.Default;
    private float _fontSize = Font.DefaultSize;
    private TextHeightMode _heightMode = Font.DefaultTextHeightMode;
    private Vector2? _sizeCache = null;
    private Vector2 _spacing = Font.DefaultTextSpacing;
    private string _value = "";

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
    public Interpolation? Interpolation { get; set; } = Drawing.DefaultInterpolation;
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;

    public string Value
    {
        get => _value;
        set
        {
            if (_value == value)
                return;
            _value = value;
            _sizeCache = null;
        }
    }

    public Font Font
    {
        get => _font;
        set
        {
            if (_font == value)
                return;
            _font = value;
            _sizeCache = null;
        }
    }

    public float FontSize
    {
        get => _fontSize;
        set
        {
            if (Precision.AreEqual(_fontSize, value))
                return;
            _fontSize = value;
            _sizeCache = null;
        }
    }

    public Vector2 Spacing
    {
        get => _spacing;
        set
        {
            if (Precision.AreEqual(_spacing, value))
                return;
            _spacing = value;
            _sizeCache = null;
        }
    }

    public TextHeightMode HeightMode
    {
        get => _heightMode;
        set
        {
            if (_heightMode == value)
                return;
            _heightMode = value;
            _sizeCache = null;
        }
    }

    public Vector2 Size => _sizeCache ??= _font.MeasureText(_value, _fontSize, _spacing, _heightMode);

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }
}
