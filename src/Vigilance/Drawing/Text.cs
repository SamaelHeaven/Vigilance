using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public struct Text
{
    public string Value = "";
    public Color Fill = Color.Transparent;
    public Color Stroke = Color.Transparent;
    public Font Font = Game.DefaultFont;
    public float FontSize = Game.DefaultFontSize;
    public float StrokeWidth = 0;
    public Vector2 Spacing = Game.DefaultTextSpacing;
    public Interpolation? Interpolation = null;
    public Func<Camera>? Camera = static () => Game.Scene.Camera;

    public Vector2 Size => Font.MeasureText(Value, FontSize, Spacing);

    public Text() { }
}
