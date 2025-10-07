using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.UI;

public class UIText : UIElement
{
    private Text _text = new();
    private TextOverflow _textOverflow;
    private string _value;

    public UIText(string value = "")
    {
        _value = value;
    }

    public UIText(string value, Color fill)
        : this(value)
    {
        Fill = fill;
    }

    public string Value
    {
        get => _value;
        set
        {
            if (value == _value)
                return;
            _value = value;
            MarkDirty();
        }
    }

    public Color Fill
    {
        get => _text.Fill;
        set => _text.Fill = value;
    }

    public Font Font
    {
        get => _text.Font;
        set
        {
            _text.Font = value;
            MarkDirty();
        }
    }

    public float FontSize
    {
        get => _text.FontSize;
        set
        {
            if (Precision.AreEqual(_text.FontSize, value))
                return;
            _text.FontSize = value;
            MarkDirty();
        }
    }

    public Color Stroke
    {
        get => _text.Stroke;
        set => _text.Stroke = value;
    }

    public float StrokeWidth
    {
        get => _text.StrokeWidth;
        set => _text.StrokeWidth = value;
    }

    public DrawOrder DrawOrder
    {
        get => _text.DrawOrder;
        set => _text.DrawOrder = value;
    }

    public Vector2 Spacing
    {
        get => _text.Spacing;
        set
        {
            if (Precision.AreEqual(_text.Spacing, value))
                return;
            _text.Spacing = value;
            MarkDirty();
        }
    }

    public TextHeightMode HeightMode
    {
        get => _text.HeightMode;
        set
        {
            if (value == _text.HeightMode)
                return;
            _text.HeightMode = value;
            MarkDirty();
        }
    }

    public Interpolation? Interpolation
    {
        get => _text.Interpolation;
        set => _text.Interpolation = value;
    }

    public TextOverflow TextOverflow
    {
        get => _textOverflow;
        set
        {
            if (value == _textOverflow)
                return;
            _textOverflow = value;
            MarkDirty();
        }
    }

    protected override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        var maxWidth = widthMode == MeasureMode.Undefined ? float.PositiveInfinity : width;
        switch (TextOverflow)
        {
            case TextOverflow.Clip:
                _text.Value = Value;
                return _text.Size;
            case TextOverflow.Ellipsis:
            {
                const string ellipsis = "...";
                var visibleText = Value;
                _text.Value = visibleText;
                var size = _text.Size;
                if (size.X <= maxWidth)
                    return size;
                var left = 0;
                var right = Value.Length;
                var result = "";
                while (left <= right)
                {
                    var mid = (left + right) / 2;
                    var candidate = string.Concat(Value.AsSpan(0, mid), ellipsis);
                    _text.Value = candidate;
                    if (_text.Size.X <= maxWidth)
                    {
                        result = candidate;
                        left = mid + 1;
                    }
                    else
                    {
                        right = mid - 1;
                    }
                }

                _text.Value = result;
                return _text.Size;
            }
            case TextOverflow.Wrap:
            default:
                var words = Value.Split(' ');
                var lines = new List<string>();
                var currentLine = "";
                foreach (var word in words)
                {
                    var line = currentLine == "" ? word : currentLine + " " + word;
                    _text.Value = line;
                    if (_text.Size.X > maxWidth && currentLine != "")
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                    }
                    else
                    {
                        currentLine = line;
                    }
                }

                if (currentLine != "")
                    lines.Add(currentLine);
                _text.Value = lines.AsValueEnumerable().JoinToString("\n");
                return _text.Size;
        }
    }

    protected override void Render(Graphics graphics, CameraProvider camera)
    {
        _text.Camera = camera;
        graphics.DrawText(LayoutPosition, _text);
    }

    protected override object DeepClone()
    {
        var result = (UIText)base.DeepClone();
        result._text = _text.DeepClone();
        return result;
    }
}
