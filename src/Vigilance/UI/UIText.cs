using LinkDotNet.StringBuilder;
using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.UI;

public class UIText : UIElement
{
    private Text _text = new();

    public UIText(string value = "")
    {
        Value = value;
    }

    public UIText(string value, Color fill)
        : this(value)
    {
        Fill = fill;
    }

    public string Value
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
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

    public Interpolation Interpolation
    {
        get => _text.Interpolation;
        set => _text.Interpolation = value;
    }

    public TextOverflow TextOverflow
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            MarkDirty();
        }
    }

    protected override void RenderSelf(Graphics graphics, CameraProvider camera)
    {
        _text.Camera = camera;
        graphics.DrawText(LayoutPosition, _text);
    }

    protected override void CloneSelf()
    {
        _text = _text.ShallowClone();
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
            {
                var initialCapacity = (int)(Value.Length * 1.25);
                using var lines =
                    initialCapacity <= 256
                        ? new ValueStringBuilder(stackalloc char[initialCapacity])
                        : new ValueStringBuilder(initialCapacity);
                using var currentLine =
                    Value.Length <= 256
                        ? new ValueStringBuilder(stackalloc char[Value.Length])
                        : new ValueStringBuilder(Value.Length);
                var any = false;
                foreach (var range in Value.AsSpan().Split(' '))
                {
                    var word = Value.AsSpan(range);
                    string line;
                    if (currentLine.IsEmpty)
                    {
                        line = word.ToString();
                    }
                    else
                    {
                        currentLine.Append(' ');
                        currentLine.Append(word);
                        line = currentLine.ToString();
                    }

                    _text.Value = line;
                    if (_text.Size.X > maxWidth && !currentLine.IsEmpty)
                    {
                        if (any)
                            lines.Append('\n');
                        lines.Append(currentLine.AsSpan());
                        currentLine.Clear();
                        currentLine.Append(word);
                        any = true;
                    }
                    else
                    {
                        currentLine.Clear();
                        currentLine.Append(line);
                    }
                }

                if (!currentLine.IsEmpty)
                {
                    if (any)
                        lines.Append('\n');
                    lines.Append(currentLine.AsSpan());
                }

                _text.Value = lines.ToString();
                return _text.Size;
            }
        }
    }
}
