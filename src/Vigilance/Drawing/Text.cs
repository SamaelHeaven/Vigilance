using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Text : IFullCloneable
{
    private Font _font = Game.DefaultFont;
    private float _fontSize = Game.DefaultFontSize;
    private TextHeightMode _heightMode = Game.DefaultTextHeightMode;
    private Vector2? _sizeCache = null;
    private Vector2 _spacing = Game.DefaultTextSpacing;
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

    public Color Fill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public Interpolation? Interpolation { get; set; } = null;
    public CameraFunc? Camera { get; set; } = Core.Camera.Default;

    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            _sizeCache = null;
        }
    }

    public Font Font
    {
        get => _font;
        set
        {
            _font = value;
            _sizeCache = null;
        }
    }

    public float FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
            _sizeCache = null;
        }
    }

    public Vector2 Spacing
    {
        get => _spacing;
        set
        {
            _spacing = value;
            _sizeCache = null;
        }
    }

    public TextHeightMode HeightMode
    {
        get => _heightMode;
        set
        {
            _heightMode = value;
            _sizeCache = null;
        }
    }

    public Vector2 Size => _sizeCache ??= _font.MeasureText(_value, _fontSize, _spacing, _heightMode);

    public override string ToString()
    {
        return Printer.Print(this);
    }
}
