using Vigilance.Core;

namespace Vigilance.Drawing;

public sealed class Circle : IFullCloneable
{
    public Circle() { }

    public Circle(Color fill)
    {
        Fill = fill;
    }

    public Color Fill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraFunc? Camera { get; set; } = Core.Camera.Default;
}
