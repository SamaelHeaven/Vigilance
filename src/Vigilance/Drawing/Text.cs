using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Text
{
    public Text() { }

    public Text(string value)
    {
        Value = value;
    }

    public string Value { get; set; } = "";
    public Color Fill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public Font Font { get; set; } = Game.DefaultFont;
    public float FontSize { get; set; } = Game.DefaultFontSize;
    public float StrokeWidth { get; set; } = 0;
    public Vector2 Spacing { get; set; } = Game.DefaultTextSpacing;
    public Interpolation? Interpolation { get; set; } = null;
    public GetCameraDelegate? Camera { get; set; } = Core.Camera.DefaultDelegate;

    public Vector2 Size => Font.MeasureText(Value, FontSize, Spacing);
}
