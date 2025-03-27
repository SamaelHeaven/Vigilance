using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public struct Text(Font font, string value = "")
{
    public string Value = value;
    public Color Fill = Color.Transparent;
    public Color Stroke = Color.Transparent;
    public Font Font = font;
    public float FontSize = Graphics.DefaultFontSize;
    public float StrokeWidth = 0;
    public float Spacing = 0;
    public Interpolation? Interpolation = null;
    public Func<Camera>? Camera = static () => Game.Scene.Camera;

    public Vector2 Size => Font.MeasureText(Value, FontSize, Spacing);
}
