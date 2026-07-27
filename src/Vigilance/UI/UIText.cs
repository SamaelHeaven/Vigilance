using LinkDotNet.StringBuilder;

namespace Vigilance.UI;

public class UIText : UIElement
{
    private Text _text = new();

    public UIText()
    {
        Value = "";
    }

    public UIText(Color fill)
        : this()
    {
        Fill = fill;
    }

    public UIText(string value)
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

    public int VisibleCharacters
    {
        get;
        set
        {
            field = value;
            MarkDirty();
        }
    } = Text.UnlimitedCharacters;

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

    public TextureFilter TextureFilter
    {
        get => _text.TextureFilter;
        set => _text.TextureFilter = value;
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

    protected override void OnRender(Graphics graphics, CameraProvider camera)
    {
        _text.Camera = camera;
        graphics.DrawText(LayoutPosition, _text);
    }

    protected override void OnClone()
    {
        _text = _text.DeepClone();
    }

    protected override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        var maxWidth = widthMode == MeasureMode.Undefined ? float.PositiveInfinity : width;
        _text.VisibleCharacters = VisibleCharacters;
        switch (TextOverflow)
        {
            case TextOverflow.Clip:
                _text.Value = Value;
                return _text.Size;
            case TextOverflow.Ellipsis:
            {
                const string ellipsis = "...";
                _text.Value = Value;
                if (_text.Size.X <= maxWidth)
                    return _text.Size;
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
            case TextOverflow.WrapCharacters:
            {
                var initialCapacity = (int)(Value.Length * 1.25f);
                using var lines =
                    initialCapacity <= 256
                        ? new ValueStringBuilder(stackalloc char[initialCapacity])
                        : new ValueStringBuilder(initialCapacity);
                using var currentLine =
                    Value.Length <= 256
                        ? new ValueStringBuilder(stackalloc char[Value.Length])
                        : new ValueStringBuilder(Value.Length);
                var hasLines = false;
                foreach (var character in Value)
                {
                    var candidate = string.Concat(currentLine.AsSpan(), character.ToString());
                    _text.Value = candidate;
                    if (_text.Size.X > maxWidth && !currentLine.IsEmpty)
                    {
                        if (hasLines)
                        {
                            if (VisibleCharacters == lines.Length)
                                _text.VisibleCharacters++;
                            lines.Append('\n');
                        }

                        lines.Append(currentLine.AsSpan());
                        currentLine.Clear();
                        currentLine.Append(character);
                        hasLines = true;
                    }
                    else
                    {
                        currentLine.Clear();
                        currentLine.Append(candidate);
                    }
                }

                if (!currentLine.IsEmpty)
                {
                    if (hasLines)
                    {
                        if (VisibleCharacters == lines.Length)
                            _text.VisibleCharacters++;
                        lines.Append('\n');
                    }

                    lines.Append(currentLine.AsSpan());
                }

                _text.Value = lines.ToString();
                return _text.Size;
            }
            case TextOverflow.WrapWords:
            default:
            {
                var initialCapacity = (int)(Value.Length * 1.25f);
                using var lines =
                    initialCapacity <= 256
                        ? new ValueStringBuilder(stackalloc char[initialCapacity])
                        : new ValueStringBuilder(initialCapacity);
                using var currentLine =
                    Value.Length <= 256
                        ? new ValueStringBuilder(stackalloc char[Value.Length])
                        : new ValueStringBuilder(Value.Length);
                var hasLines = false;
                foreach (var range in Value.AsSpan().Split(' '))
                {
                    var word = Value.AsSpan(range);
                    var candidate = currentLine.IsEmpty
                        ? word.ToString()
                        : string.Concat(currentLine.AsSpan(), " ", word);
                    _text.Value = candidate;
                    if (_text.Size.X > maxWidth && !currentLine.IsEmpty)
                    {
                        if (hasLines)
                        {
                            if (VisibleCharacters == lines.Length)
                                _text.VisibleCharacters++;
                            lines.Append('\n');
                        }

                        lines.Append(currentLine.AsSpan());
                        currentLine.Clear();
                        currentLine.Append(word);
                        hasLines = true;
                    }
                    else
                    {
                        currentLine.Clear();
                        currentLine.Append(candidate);
                    }
                }

                if (!currentLine.IsEmpty)
                {
                    if (hasLines)
                    {
                        if (VisibleCharacters == lines.Length)
                            _text.VisibleCharacters++;
                        lines.Append('\n');
                    }

                    lines.Append(currentLine.AsSpan());
                }

                _text.Value = lines.ToString();
                return _text.Size;
            }
        }
    }
}
